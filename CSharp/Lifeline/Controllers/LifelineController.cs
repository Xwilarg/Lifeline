using Lifeline.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Lifeline.Controllers
{
    [ApiController]
    [Route("/lifeline/")]
    public class LifelineController(Database database, JsonSerializerOptions opts) : ControllerBase
    {
        [HttpPost("init")]
        public ActionResult Init()
        {
            var path = PathHelper.DataPath + "config.json";
            if (!System.IO.File.Exists(path)) return StatusCode(StatusCodes.Status412PreconditionFailed, "Missing config.json");
            var data = JsonSerializer.Deserialize<Config[]>(path, opts);
            if (data == null) return StatusCode(StatusCodes.Status412PreconditionFailed, "Invalid config.json");

            using var transaction = database.Database.BeginTransaction();
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
                        Path = c.Path
                    });
                }
            }
            transaction.Commit();

            return StatusCode(StatusCodes.Status201Created);
        }
        [HttpPost("status")]
        public ActionResult Get()
        {
            return StatusCode(StatusCodes.Status200OK, database.Lifelines.Select(x => new LifelineResponse()
            {
                Id = x.Id,
                Name = x.Name,
                Data = x.Path != null && System.IO.File.Exists(x.Path) ? System.IO.File.ReadAllText(x.Path),
                LastInsert = x.LastInsert
            }));
        }
    }
}
