using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vizsgaremek
{
    [Table("points")]
    public class Points
    {
        [Key]
        public int id { get; set; }
        public int pointsWordle { get; set; }
        public int pointsSnake { get; set; }
        public int pointsFlappyBird { get; set; }
        public int userId { get; set; }
    }
}