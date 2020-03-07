using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NostalgiaGenerator.AppCode;
using NostalgiaGenerator.Models;
using QuickType;

namespace NostalgiaGenerator.Controllers
{
    public class MusicController : Controller
    {
        private readonly ILogger<MusicController> _logger;
        private readonly string _spotifyClientId;
        private readonly string _spotifyClientSecret;

        public MusicController(ILogger<MusicController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _spotifyClientId = configuration["SpotifyClientId"];
            _spotifyClientSecret = configuration["SpotifyClientSecret"];
        }

        public IActionResult MusicSelect()
        {
            return View(new MusicSelectModel());
        }

        [HttpPost]
        public IActionResult MusicSelect(int year)
        {
            SpotifyApiController spotify = new SpotifyApiController();
            string token = spotify.GetClientCredentialsAuthToken(_spotifyClientId, _spotifyClientSecret);
            List<Track> tracks = spotify.GetBillboardTopByYear(token, year);

            return View("MusicListen", new MusicListenModel(tracks, year));
        }

        [HttpPost]
        public IActionResult MusicSelectHipster(int year)
        {
            SpotifyApiController spotify = new SpotifyApiController();
            string token = spotify.GetClientCredentialsAuthToken(_spotifyClientId, _spotifyClientSecret);
            List<Track> tracks = spotify.GetHipsterMusicByYear(token, year);

            return View("MusicListen", new MusicListenModel(tracks, year));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
