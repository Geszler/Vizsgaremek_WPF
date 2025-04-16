using System.Configuration;
using System.Data;
using System.Windows;
using Vizsgaremek.ApiServices;

namespace Vizsgaremek
{
    public partial class App : Application
    {
        public static LoggedInUser? CurrentUser { get; set; }
    }
}
