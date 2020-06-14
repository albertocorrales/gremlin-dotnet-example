using GremlinExampleApi.Models;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Remote;
using Gremlin.Net.Process.Traversal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Gremlin.Net.Process.Traversal.AnonymousTraversalSource;

namespace GremlinExampleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        protected readonly GraphTraversalSource _g;

        public PersonController(IConfiguration config)
        {
            var server = new GremlinServer(
                config.GetValue<string>("Gremlin:Hostname"),
                config.GetValue<int>("Gremlin:Port"),
                config.GetValue<bool>("Gremlin:EnableSsl"));

            var client = new GremlinClient(server);
            _g = Traversal().WithRemote(new DriverRemoteConnection(client));
        }

        // GET: api/person
        [HttpGet]
        public async Task<IEnumerable<Person>> Get()
        {

            var people = new List<Person>();

            var peopleProperties = (await _g.V()
                .ValueMap<string, object[]>()
                .Promise(t => t.ToList()));

            foreach (var personProperties in peopleProperties)
            {
                people.Add(new Person()
                {
                    Id = personProperties["key"].First().ToString(),
                    Name = personProperties["name"].First().ToString()
                });
            }

            return people;
        }

        // GET: api/Person/5
        [HttpGet("{id}")]
        public async Task<Person> Get(int id)
        {
            var properties = (await _g.V()
                .Has("key", id)
                .ValueMap<string, object[]>()
                .Promise(t => t.ToList())).FirstOrDefault();

            return new Person()
            {
                Id = properties["key"].First().ToString(),
                Name = properties["name"].First().ToString()
            };
        }

        // POST: api/Person
        [HttpPost]
        public async Task Post([FromBody] Person person)
        {
            await _g.AddV("person")
                .Property("key", person.Id)
                .Property("name", person.Name)
                .Promise(t => t.Next());
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _g.V()
                .Has("key", id)
                .Drop()
                .Promise(t => t.Next());
        }
    }
}
