using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Xamarin.Forms;

namespace Pictionary
{
    /// <summary>
    /// <para>Contains Picture url, word in language1 and the meanings in language2</para>
    /// <para>The ListView of the UserMainPage contains WordViewModel objects</para>
    /// </summary>
    public class WordViewModel
    {
        public string Id { get; set; }
        public string ImageId { get; set; }
        public string Picture { get; set; }
        public string Word { get; set; }
        public List<string> Meaning { get; set; } = new List<string>();
        public string MeaningStr { get; set; }
    }
}
