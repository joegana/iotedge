// Copyright (c) Microsoft. All rights reserved.
namespace TwinTester
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Edge.ModuleUtil;
    using Microsoft.Azure.Devices.Edge.ModuleUtil.TestResultCoordinatorClient;
    using Microsoft.Azure.Devices.Edge.Util;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Logging;

    class TwinEdgeOperationsResultHandler : ITwinTestResultHandler
    {
        static readonly ILogger Logger = ModuleUtil.CreateLogger(nameof(TwinEdgeOperationsResultHandler));
        readonly TestResultCoordinatorClient trcClient;
        readonly string moduleId;
        readonly string trackingId;

        public TwinEdgeOperationsResultHandler(Uri reporterUri, string moduleId, Option<string> trackingId)
        {
            this.trcClient = new TestResultCoordinatorClient() { BaseUrl = reporterUri.AbsoluteUri };
            this.moduleId = moduleId;
            this.trackingId = trackingId.Expect(() => new ArgumentNullException(nameof(trackingId)));
        }

        public Task HandleDesiredPropertyReceivedAsync(TwinCollection desiredProperties)
        {
            return this.SendReportAsync($"{this.moduleId}.desiredReceived", StatusCode.DesiredPropertyReceived, desiredProperties);
        }

        public Task HandleDesiredPropertyUpdateAsync(string propertyKey)
        {
            TwinCollection properties = this.GetTwinCollection(propertyKey);
            return this.SendReportAsync($"{this.moduleId}.desiredUpdated", StatusCode.DesiredPropertyUpdated, properties);
        }

        public Task HandleReportedPropertyUpdateAsync(string propertyKey)
        {
            TwinCollection properties = this.GetTwinCollection(propertyKey);
            return this.SendReportAsync($"{this.moduleId}.reportedUpdated", StatusCode.ReportedPropertyUpdated, properties);
        }

        public Task HandleTwinValidationStatusAsync(string status)
        {
            return Task.CompletedTask;
        }

        public Task HandleReportedPropertyUpdateExceptionAsync(string failureStatus)
        {
            return Task.CompletedTask;
        }

        TwinCollection GetTwinCollection(string propertyKey)
        {
            var properties = new TwinCollection();
            properties[propertyKey] = propertyKey;

            return properties;
        }

        async Task SendReportAsync(string source, StatusCode statusCode, TwinCollection details, string exception = "")
        {
            var result = new TwinTestResult() { Operation = statusCode.ToString(), Properties = details, ErrorMessage = exception, TrackingId = this.trackingId };
            Logger.LogDebug($"Sending report {result.ToString()}");
            await ModuleUtil.ReportStatus(this.trcClient, Logger, source, result.ToString(), TestOperationResultType.Twin.ToString());
        }
    }
}
