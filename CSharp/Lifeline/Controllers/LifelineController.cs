using Lifeline.Models;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("status")]
        public ActionResult Get()
        {
            return StatusCode(StatusCodes.Status200OK, database.Lifelines.Select(x => new LifelineResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Data = x.Path != null && System.IO.File.Exists(x.Path) ? System.IO.File.ReadAllText(x.Path) : null,
                LastInsert = x.LastInsert
            }));
        }

        [HttpPost("receive")]
        public ActionResult Receive([FromBody] LifelineRequest body)
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

            if (target.Path != null && System.IO.File.Exists(target.Path)) return new JsonResult(System.IO.File.ReadAllText(target.Path));
            return StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost("send")]
        public async Task<ActionResult> Send()
        {
            var tasks = database.Lifelines.ToList().Select(async x =>
            {
                var id = RandomNumberGenerator.GetString("abcdefghijklmnopqrstuvwxyz0123456789", 20);
                using var res = await client.PostAsJsonAsync(x.Target, new LifelineRequest()
                {
                    Id = id,
                    Token = x.Token,
                    Data = x.Path != null && System.IO.File.Exists(x.Path) ? System.IO.File.ReadAllText(x.Path) : null
                }, opts);
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    return (id, x.Token, JsonSerializer.Deserialize<string>(await res.Content.ReadAsStringAsync()));
                }
                return (id, x.Token, null);
            });

            var results = await Task.WhenAll(tasks);

            foreach (var r in results)
            {
                var curr = database.Lifelines.First(x => x.Token == r.Token);
                curr.Data = r.Item3;
                curr.LastInsert = DateTime.UtcNow;
                curr.Id = r.id;
            }
            database.SaveChanges();

            return StatusCode(StatusCodes.Status204NoContent);
        }
    }
}
