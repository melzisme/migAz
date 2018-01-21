﻿using MigAz.Azure;
using MigAz.Tests.Fakes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigAz.Azure.Asm;
using MigAz.Azure.Arm;
using MigAz.Azure.Generator.AsmToArm;
using MigAz.Core.Interface;
using MigAz.Core.Generator;
using MIGAZ.Tests.Fakes;

namespace MigAz.Tests
{
    static class TestHelper
    {
        public const string TenantId = "11111111-1111-1111-1111-111111111111";
        public const string SubscriptionId = "22222222-2222-2222-2222-222222222222";
        public static ISubscription GetTestAzureSubscription()
        {
            return new FakeAzureSubscription();
        }
        public static async Task<AzureContext> SetupAzureContext(string restResponseFile)
        {
            return await SetupAzureContext(AzureEnvironment.AzureCloud, restResponseFile);
        }

        public static async Task<AzureContext> SetupAzureContext(AzureEnvironment azureEnvironment, string restResponseFile)
        {
            ILogProvider logProvider = new FakeLogProvider();
            IStatusProvider statusProvider = new FakeStatusProvider();
            ISettingsProvider settingsProvider = new FakeSettingsProvider();
            AzureContext azureContext = new AzureContext(logProvider, statusProvider, settingsProvider);
            azureContext.AzureEnvironment = azureEnvironment;
            azureContext.TokenProvider = new FakeTokenProvider();
            azureContext.AzureRetriever = new TestRetriever(azureContext);
            azureContext.AzureRetriever.LoadRestCache(restResponseFile);

            List<AzureSubscription> subscriptions = await azureContext.AzureRetriever.GetAzureARMSubscriptions();
            await azureContext.SetSubscriptionContext(subscriptions[0]);
            await azureContext.AzureRetriever.SetSubscriptionContext(subscriptions[0]);

            return azureContext;
        }

        internal static void SetTargetSubnets(ExportArtifacts artifacts)
        {
            string x = "{\r\n  \"name\": \"DummyVNet\",\r\n  \"id\": \"/subscriptions/" + SubscriptionId + "/resourceGroups/dummygroup-rg/providers/Microsoft.Network/virtualNetworks/DummyVNet\",\r\n  \"etag\": \"W/\\\"1fa3c5bd-1cf4-4bb9-9839-96ece3b3776d\\\"\",\r\n  \"type\": \"Microsoft.Network/virtualNetworks\",\r\n  \"location\": \"westus\",\r\n  \"properties\": {\r\n    \"provisioningState\": \"Succeeded\",\r\n    \"resourceGuid\": \"b8b6b69d-2480-436a-886b-ac3ef4061253\",\r\n    \"addressSpace\": {\r\n      \"addressPrefixes\": [\r\n        \"10.0.0.0/16\"\r\n      ]\r\n    },\r\n    \"subnets\": [\r\n      {\r\n        \"name\": \"subnet01\",\r\n        \"id\": \"/subscriptions/" + SubscriptionId + "/resourceGroups/dummygroup-rg/providers/Microsoft.Network/virtualNetworks/DummyVNet/subnets/subnet01\",\r\n        \"etag\": \"W/\\\"1f53c5be-1cf4-4bb9-9839-96ece3b3776d\\\"\",\r\n        \"properties\": {\r\n          \"provisioningState\": \"Succeeded\",\r\n          \"addressPrefix\": \"10.0.0.0/24\",\r\n\"applicationGatewayIPConfigurations\": [\r\n            {\r\n              \"id\": \"/subscriptions/" + SubscriptionId + "/resourceGroups/dummygroup-rg/providers/Microsoft.Network/applicationGateways/appgwtest/gatewayIPConfigurations/gatewayIP01\"\r\n            }\r\n          ]\r\n        }\r\n      }\r\n    ]\r\n  }\r\n}";
            JObject webRequestResultJson = JObject.Parse(x);

            // Azure.Arm.VirtualNetwork armVirtualNetwork = new Azure.Arm.VirtualNetwork(webRequestResultJson);

            // todo??
            //foreach (Azure.Asm.VirtualMachine asmVirtualMachine in artifacts.VirtualMachines)
            //{
            //    if (asmVirtualMachine.TargetVirtualNetwork == null)
            //        asmVirtualMachine.TargetVirtualNetwork = armVirtualNetwork;

            //    if (asmVirtualMachine.TargetSubnet == null)
            //        asmVirtualMachine.TargetSubnet = armVirtualNetwork.Subnets[0];
            //}
        }
        
        public static async Task<AzureGenerator> SetupTemplateGenerator(AzureContext azureContext)
        {
            ITelemetryProvider telemetryProvider = new FakeTelemetryProvider();
            return new AzureGenerator(TestHelper.GetTestAzureSubscription(), TestHelper.GetTestAzureSubscription(), azureContext.LogProvider, azureContext.StatusProvider, telemetryProvider);
        }

        public static JObject GetJsonData(MemoryStream closedStream)
        {
            var newStream = new MemoryStream(closedStream.ToArray());
            var reader = new StreamReader(newStream);
            var templateText = reader.ReadToEnd();
            return JObject.Parse(templateText);
        }

        internal static async Task<Azure.MigrationTarget.ResourceGroup> GetTargetResourceGroup(AzureContext azureContext)
        {
            List<Azure.Arm.Location> azureLocations = await azureContext.AzureSubscription.GetAzureARMLocations();
            Azure.MigrationTarget.ResourceGroup targetResourceGroup = new Azure.MigrationTarget.ResourceGroup();
            targetResourceGroup.TargetLocation = azureLocations[0];
            return targetResourceGroup;
        }
    }
}
