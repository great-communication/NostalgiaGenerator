using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NostalgiaGenerator.AppCode
{
    public class SpotifyAccountsServiceModel
    {
        public string access_token;

        public string token_type;

        public string scope;

        public int expires_in;

        public string refresh_token;
    }
}