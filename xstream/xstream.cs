using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;

namespace xstream
{
    public partial class xstream : Form
    {
        public xstream()
        {
            InitializeComponent();

            string reqURL = AuthenticationService.GetWindowsLiveAuthenticationUrl();
            WindowsLiveResponse rep = AuthenticationService.ParseWindowsLiveResponse("");
            AuthenticationService auth = new AuthenticationService(rep);

            if (!auth.Authenticate())
                throw new Exception("Authentication failed!");

            Console.WriteLine(auth.XToken);
            Console.WriteLine(auth.UserInformation);
        }

    }
}
