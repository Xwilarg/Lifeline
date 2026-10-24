using Lifeline.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Lifeline.Controllers
{
    [ApiController]
    [Route("/lifeline/")]
    public class LifelineController(Database database, JsonSerializerOptions opts, HttpClient client) : ControllerBase
    {
        [HttpPost("init")]
        public ActionResult Init()
        {
            var path = PathHelper.DataPath + "config.json";
            if (!System.IO.File.Exists(path)) return StatusCode(StatusCodes.Status412PreconditionFailed, "Missing config.json");
            var data = JsonSerializer.Deserialize<Config[]>(System.IO.File.ReadAllText(path), opts);
            if (data == null) return StatusCode(StatusCodes.Status412PreconditionFailed, "Invalid config.json");

            foreach (var c in data)
            {
                if (!database.Lifelines.Any(x => x.Name == c.Name))
                {
                    database.Lifelines.Add(new()
                    {
                        Key = BitConverter.ToInt32(SHA1.HashData(Encoding.UTF8.GetBytes(c.Name))),
                        Name = c.Name,
                        Target = c.Target,
                        Id = null,
                        Token = c.Token,
                        LastInsert = DateTime.UtcNow,
                        Path = c.Path,
                        Data = null
                    });
                }
            }
            database.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpGet("status")]
        public ActionResult Get()
        {
            return StatusCode(StatusCodes.Status200OK, database.Lifelines.Select(x => new LifelineResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Data = x.Data,
                LastInsert = x.LastInsert.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            }));
        }

        [HttpPost("receive")]
        public ActionResult Receive([FromForm] LifelineRequest body)
        {
            var target = database.Lifelines.FirstOrDefault(x => body.Token == x.Token);
            if (target == null)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, "Invalid token");
            }

            target.Data = body.Data;
            target.Id = body.Id;
            target.LastInsert = DateTime.UtcNow;
            database.SaveChanges();

            if (target.Path != null && System.IO.File.Exists(target.Path)) return StatusCode(StatusCodes.Status200OK, System.IO.File.ReadAllText(target.Path));
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost("send")]
        public async Task<ActionResult> Send()
        {
            var lifelines = database.Lifelines.ToList();
            var tasks = lifelines.Select(async x =>
            {
                var id = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyz0123456789", 20);
                var data = x.Path != null && System.IO.File.Exists(x.Path) ? System.IO.File.ReadAllText(x.Path) : null;
                var fields = new Dictionary<string, string> { ["id"] = id, ["token"] = x.Token };
                if (data != null) fields.Add("data", data);

                try
                {
                    using var res = await client.PostAsync(x.Target, new FormUrlEncodedContent(fields));
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        return ((string?)id, x.Token, (string?)await res.Content.ReadAsStringAsync());
                    }
                    if (res.StatusCode == HttpStatusCode.NoContent)
                    {
                        return ((string?)id, x.Token, null);
                    }
                    return (null, x.Token, null);
                }
                catch
                {
                    return (null, x.Token, null);
                }
            });

            var results = await Task.WhenAll(tasks);

            foreach (var r in results)
            {
                var curr = database.Lifelines.First(x => x.Token == r.Token);
                curr.Data = r.Item3;
                curr.LastInsert = DateTime.UtcNow;
                curr.Id = r.Item1;
            }
            database.SaveChanges();

            return StatusCode(StatusCodes.Status200OK, lifelines.ToDictionary(x => x.Name, x => new LifelineResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Data = x.Data,
                LastInsert = x.LastInsert.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            }));
        }
    }
}
