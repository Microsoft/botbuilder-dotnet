﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Bot.Connector.Authentication
{
    public static class ChannelValidation
    {
        // This claim is ONLY used in the Channel Validation, and not in the emulator validation        
        private const string ServiceUrlClaim = "serviceurl";

        /// <summary>
        /// TO BOT FROM CHANNEL: Token validation parameters when connecting to a bot
        /// </summary>
        public static readonly TokenValidationParameters ToBotFromChannelTokenValidationParameters =
            new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { AuthorizationConstants.BotFrameworkTokenIssuer },
                // Audience validation takes place in JwtTokenExtractor
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true
            };

        /// <summary>
        /// Validate the incoming Auth Header as a token sent from the Bot Framework Service.
        /// </summary>
        /// <remarks>
        /// A token issued by the Bot Framework emulator will FAIL this check. 
        /// </remarks>
        /// <param name="authHeader">The raw HTTP header in the format: "Bearer [longString]"</param>
        /// <param name="credentials">The user defined set of valid credentials, such as the AppId.</param>
        /// <param name="serviceUrl">The token contains a "serviceUrl" claim with value that matches the 
        /// servieUrl property that came in via the Activity object of the incoming request</param>
        /// <returns>
        /// A valid ClaimsIdentity. 
        /// </returns>
        public static async Task<ClaimsIdentity> AuthenticateChannelToken(string authHeader, ICredentialProvider credentials, string serviceUrl)
        {
            var tokenExtractor = new JwtTokenExtractor(
                  ToBotFromChannelTokenValidationParameters,
                  AuthorizationConstants.ToBotFromChannelOpenIdMetadataUrl,
                  AuthorizationConstants.AllowedSigningAlgorithms, null);

            var identity = await tokenExtractor.GetIdentityAsync(authHeader);
            if (identity == null)
            {
                // No valid identity. Not Authorized. 
                throw new UnauthorizedAccessException();
            }

            if (!identity.IsAuthenticated)
            {
                // The token is in some way invalid. Not Authorized. 
                throw new UnauthorizedAccessException();
            }

            var serviceUrlClaim = identity.Claims.FirstOrDefault(claim => claim.Type == ServiceUrlClaim)?.Value;
            if (string.IsNullOrWhiteSpace(serviceUrlClaim))
            {
                // Claim must be present. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            if (!string.Equals(serviceUrlClaim, serviceUrl))
            {
                // Claim must match. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            // Now check that the AppID in the claimset matches
            // what we're looking for. Note that in a multi-tenant bot, this value
            // comes from developer code that may be reaching out to a service, hence the 
            // Async validation. 

            // Look for the "aud" claim, but only if issued from the Bot Framework
            Claim audianceClaim = identity.Claims.FirstOrDefault(
                c => c.Issuer == AuthorizationConstants.BotFrameworkTokenIssuer && c.Type == AuthorizationConstants.AudienceClaim);

            if (audianceClaim == null)
            {
                // The relevant Audiance Claim MUST be present. Not Authorized.
                throw new UnauthorizedAccessException();
            }

            // The AppId from the claim in the token must match the AppId specified by the developer. Note that
            // the Bot Framwork uses the Audiance claim ("aud") to pass the AppID. 
            string appIdFromClaim = audianceClaim.Value;
            if (string.IsNullOrWhiteSpace(appIdFromClaim))
            {
                // Claim is present, but doesn't have a value. Not Authorized. 
                throw new UnauthorizedAccessException();
            }

            if (!await credentials.IsValidAppIdAsync(appIdFromClaim))
            {
                // The AppId is not valid. Not Authorized. 
                throw new UnauthorizedAccessException($"Invalid AppId passed on token: {appIdFromClaim}");
            }

            return identity;
        }
    }
}
