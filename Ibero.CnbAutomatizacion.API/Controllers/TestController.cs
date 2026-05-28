using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ibero.CnbAutomatizacion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        // GET: api/test/ping
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "API funcionando", date = DateTime.Now });
        }

        // GET: api/test/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return Ok(new { id, name = $"Item {id}" });
        }

        // POST: api/test
        [HttpPost]
        public IActionResult Create([FromBody] object data)
        {
            return Ok(new
            {
                message = "Recibido correctamente",
                data
            });
        }

        // PUT: api/test/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] object data)
        {
            return Ok(new
            {
                message = $"Actualizado {id}",
                data
            });
        }

        // DELETE: api/test/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok(new { message = $"Eliminado {id}" });
        }

        // GET: api/test/headers
        [HttpGet("headers")]
        public IActionResult GetHeaders()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            return Ok(headers);
        }

        // POST: api/test/form
        [HttpPost("form")]
        public IActionResult PostForm([FromForm] string name, [FromForm] IFormFile file)
        {
            return Ok(new
            {
                name,
                fileName = file?.FileName,
                size = file?.Length
            });
        }

        // GET: api/test/error
        [HttpGet("error")]
        public IActionResult Error()
        {
            throw new Exception("Error de prueba");
        }
    }
}