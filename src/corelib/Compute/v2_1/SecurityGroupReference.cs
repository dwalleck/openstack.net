﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary />   
    public class SecurityGroupReference : IHaveExtraData, IServiceResource
    {
        /// <summary />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary />
        [JsonExtensionData]
        IDictionary<string, JToken> IHaveExtraData.Data { get; set; } = new Dictionary<string, JToken>();

        object IServiceResource.Owner { get; set; }

        private async Task<SecurityGroup> LoadSecurityGroup(CancellationToken cancellationToken)
        {
            var securityGroup = this as SecurityGroup;
            if (securityGroup != null)
                return securityGroup;

            var owner = this.TryGetOwner<ComputeApiBuilder>();

            // In some cases, such as when working with the groups on a server, we only have the name and not the id
            var groups = await owner.ListSecurityGroupsAsync<SecurityGroupCollection>(cancellationToken: cancellationToken);
            securityGroup = groups.FirstOrDefault(x => x.Name == Name);
            if(securityGroup == null)
                throw new Exception($"Unable to find the security group named: {Name}.");

            return securityGroup;
        }

        /// <inheritdoc cref="ComputeApiBuilder.GetSecurityGroupAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<SecurityGroup> GetSecurityGroupAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await LoadSecurityGroup(cancellationToken);
        }

        /// <inheritdoc cref="ComputeApiBuilder.DeleteSecurityGroupAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.TryGetOwner<ComputeApiBuilder>();
            var securityGroup = await LoadSecurityGroup(cancellationToken);

            await owner.DeleteSecurityGroupAsync(securityGroup.Id, cancellationToken);
        }
    }
}