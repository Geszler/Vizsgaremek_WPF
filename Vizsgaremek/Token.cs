using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Vizsgaremek
{
    [Table("token")]
    public class Token
    {
        [Key]
        public string token { get; set; }
        public int userId { get; set; }
    }
}

