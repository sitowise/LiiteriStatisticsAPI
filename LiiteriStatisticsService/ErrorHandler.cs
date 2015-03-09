using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

//using System.Collections.ObjectModel;
using System.Diagnostics;

using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace LiiteriStatisticsService
{
    public class Log4NetErrorHandler : IErrorHandler
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool HandleError(Exception error)
        {
            logger.Error("An unexpected has occurred.", error);

            return false; // Exception has to pass the stack further
        }

        public void ProvideFault(
            Exception error, MessageVersion version, ref Message fault)
        {
        }
    }

    public class Log4NetServiceBehavior : IServiceBehavior
    {
        public void AddBindingParameters(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase,
            System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            IErrorHandler errorHandler = new Log4NetErrorHandler();

            foreach (ChannelDispatcher channelDispatcher in
                    serviceHostBase.ChannelDispatchers) {
                channelDispatcher.ErrorHandlers.Add(errorHandler);
            }
        }

        public void Validate(
            ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
        }
    }

    public class Log4NetBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(Log4NetServiceBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new Log4NetServiceBehavior();
        }
    }
}