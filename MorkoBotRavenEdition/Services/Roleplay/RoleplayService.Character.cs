using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MorkoBotRavenEdition.Models.Roleplay;

namespace MorkoBotRavenEdition.Services.Roleplay
{
#if ROLEPLAY_ENABLED
    internal partial class RoleplayService
    {
        public async Task<RoleplayCharacter> NewCharacter()
        {
            return null;
        }
    }
#endif
}
