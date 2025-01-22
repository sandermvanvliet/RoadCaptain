// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System.Threading.Tasks;

namespace RoadCaptain.Ports
{
    public interface ISecurityTokenProvider
    {
        Task<string?> GetSecurityTokenForPurposeAsync(TokenPurpose purpose, TokenPromptBehaviour promptBehaviour);
    }
}
