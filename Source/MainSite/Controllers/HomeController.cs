using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MainSite.Models;
using Microsoft.AspNetCore.Authorization;
using System.Net.Sockets;
using System.Collections.Immutable;
using NexusForever.Database.Auth;
using NexusForever.Shared.Cryptography;
using NexusForever.Shared.Database;
using NexusForever.Database.Auth.Model;

namespace MainSite.Controllers
{
    public class HomeController : Controller
    {
        private bool isOnline = false;
        private ImmutableList<ServerModel> servers;

        public IActionResult Index()
        {
            GetStatusImage();
            return View();
        }
        

        
        public IActionResult Register(AccountBaseModel newUser)
        {
            
            GetStatusImage();
            if (newUser.Email != null && newUser.Confirmation != null && newUser.Password != null)
            {
                if (newUser.Password.Equals(newUser.Confirmation))
                {
                    try
                    {
                        (string salt, string verifier) = PasswordProvider.GenerateSaltAndVerifier(newUser.Email, newUser.Password);
                        DatabaseManager.Instance.AuthDatabase.CreateAccount(newUser.Email.ToLower(), salt, verifier);
                        return View("RegisterSuccess");
                    }
                    catch (Exception e)
                    {
                        return View("DBException");
                    }
                }
                return View("RegisterFailed");

            }
            return View("Index");
        }

        private void GetStatusImage()
        {
            try
            {
                if (servers == null)
                {
                    servers = GetServers();
                }

                isOnline = PingHost(servers.First().Host, servers.First().Port);

                switch (isOnline)
                {
                    case true:
                        ViewBag.StatusSrc = "statusOn.png";
                        break;
                    default:
                        ViewBag.StatusSrc = "statusOff.png";
                        break;

                }
            }
            catch
            {
                ViewBag.StatusSrc = "statusOff.png";
            }
        }

        private static bool PingHost(string hostIP, int portNr)
        {
            try
            {
                using (var client = new TcpClient(hostIP, portNr))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static ImmutableList<ServerModel> GetServers()
        {
            return DatabaseManager.Instance.AuthDatabase.GetServers();
        }
    }
}
