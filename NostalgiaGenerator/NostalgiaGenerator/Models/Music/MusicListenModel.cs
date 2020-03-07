using QuickType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NostalgiaGenerator.Models
{
    public class MusicListenModel
    {
        public string Message = string.Empty;

        public List<Track> Tracks;

        public int Year;

        public MusicListenModel(List<Track> tracks, int year)
        {
            if(tracks.Count == 0)
            {
                Message = "Sorry could not find any tracks.";
            }            

            Tracks = tracks;
            Year = year;
        }
    }
}
