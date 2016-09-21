using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace CustomMiddleware
{
    public class SOAPMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Type _serviceType;
        private readonly string _endpointPath;
        private readonly MessageEncoder _messageEncoder;
        private readonly ServiceDescription _service;
        private IServiceProvider serviceProvider;

        public SOAPMiddleware(RequestDelegate next, Type serviceType, string path, MessageEncoder encoder,IServiceProvider _serviceProvider)
        {
            _next = next;
            _serviceType = serviceType;
            _endpointPath = path;
            _messageEncoder = encoder;
            _service = new ServiceDescription(serviceType);
            serviceProvider = _serviceProvider;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Equals(_endpointPath, StringComparison.Ordinal))
            {
                Message responseMessage;

                //读取Request请求信息
                var requestMessage = _messageEncoder.ReadMessage(httpContext.Request.Body, 0x10000, httpContext.Request.ContentType);
                var soapAction = httpContext.Request.Headers["SOAPAction"].ToString().Trim('\"');
                if (!string.IsNullOrEmpty(soapAction))
                {
                    requestMessage.Headers.Action = soapAction;
                }
                //获取操作
                var operation = _service.Operations.Where(o => o.SoapAction.Equals(requestMessage.Headers.Action, StringComparison.Ordinal)).FirstOrDefault();
                if (operation == null)
                {
                    throw new InvalidOperationException($"No operation found for specified action: {requestMessage.Headers.Action}");
                }
                //获取注入的服务
                var serviceInstance = serviceProvider.GetService(_service.ServiceType);

                //获取操作的参数信息
                var arguments = GetRequestArguments(requestMessage, operation);

                // 执行操作方法
                var responseObject = operation.DispatchMethod.Invoke(serviceInstance, arguments.ToArray());

                var resultName = operation.DispatchMethod.ReturnParameter.GetCustomAttribute<MessageParameterAttribute>()?.Name ?? operation.Name + "Result";
                var bodyWriter = new ServiceBodyWriter(operation.Contract.Namespace, operation.Name + "Response", resultName, responseObject);
                responseMessage = Message.CreateMessage(_messageEncoder.MessageVersion, operation.ReplyAction, bodyWriter);

                httpContext.Response.ContentType = httpContext.Request.ContentType; // _messageEncoder.ContentType;
                httpContext.Response.Headers["SOAPAction"] = responseMessage.Headers.Action;

                _messageEncoder.WriteMessage(responseMessage, httpContext.Response.Body);
            }
            else
            {
                await _next(httpContext);
            }
        }

        private object[] GetRequestArguments(Message requestMessage, OperationDescription operation)
        {
            var parameters = operation.DispatchMethod.GetParameters();
            var arguments = new List<object>();

            // 反序列化请求包和对象
            using (var xmlReader = requestMessage.GetReaderAtBodyContents())
            {
                // 查找的操作数据的元素
                xmlReader.ReadStartElement(operation.Name, operation.Contract.Namespace);

                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameterName = parameters[i].GetCustomAttribute<MessageParameterAttribute>()?.Name ?? parameters[i].Name;
                    xmlReader.MoveToStartElement(parameterName, operation.Contract.Namespace);
                    if (xmlReader.IsStartElement(parameterName, operation.Contract.Namespace))
                    {
                        var serializer = new DataContractSerializer(parameters[i].ParameterType, parameterName, operation.Contract.Namespace);
                        arguments.Add(serializer.ReadObject(xmlReader, verifyObjectName: true));
                    }
                }
            }

            return arguments.ToArray();
        }
    }

    public static class SOAPMiddlewareExtensions
    {
        public static IApplicationBuilder UseSOAPMiddleware<T>(this IApplicationBuilder builder, string path, MessageEncoder encoder)
        {
            return builder.UseMiddleware<SOAPMiddleware>(typeof(T), path, encoder);
        }
        public static IApplicationBuilder UseSOAPMiddleware<T>(this IApplicationBuilder builder, string path, Binding binding)
        {
            var encoder = binding.CreateBindingElements().Find<MessageEncodingBindingElement>()?.CreateMessageEncoderFactory().Encoder;
            return builder.UseMiddleware<SOAPMiddleware>(typeof(T), path, encoder);
        }
    }
}
