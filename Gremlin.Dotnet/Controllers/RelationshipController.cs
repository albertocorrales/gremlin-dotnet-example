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
    public class RelationshipController : ControllerBase
    {
        protected readonly GraphTraversalSource _g;

        public RelationshipController(IConfiguration config)
        {
            var server = new GremlinServer(
                config.GetValue<string>("Gremlin:Hostname"),
                config.GetValue<int>("Gremlin:Port"),
                config.GetValue<bool>("Gremlin:EnableSsl"));

            var client = new GremlinClient(server);
            _g = Traversal().WithRemote(new DriverRemoteConnection(client));
        }

        // GET: api/relationship
        [HttpGet]
        public async Task<IEnumerable<Relationship>> GetRelationships()
        {

            var people = new List<Relationship>();

            var peopleProperties = (await _g.E()
                .ValueMap<string, object[]>()
                .Promise(t => t.ToList()));

            foreach (var RelationshipProperties in peopleProperties)
            {
                people.Add(new Relationship()
                {
                    Id = RelationshipProperties["key"].First().ToString(),
                });
            }

            return people;
        }

        // GET: api/relationship/5
        [HttpGet("{id}")]
        public async Task<Relationship> Get(int id)
        {
            var properties = (await _g.E()
                .Has("key", id)
                .ValueMap<string, object[]>()
                .Promise(t => t.ToList())).FirstOrDefault();

            return new Relationship()
            {
                Id = properties["key"].First().ToString(),
                SourcePersonId = properties["sourceId"].First().ToString(),
                TargetPersonId = properties["targetId"].First().ToString(),
                Type = properties["type"].First().ToString(),
            };
        }

        // POST: api/relationship
        [HttpPost]
        public async Task Post([FromBody] Relationship relationship)
        {
            var source = await _g.V().Has("key", relationship.SourcePersonId).Promise(t => t.Next());
            var target = await _g.V().Has("key", relationship.TargetPersonId).Promise(t => t.Next());

            await _g.V(source)
                .AddE(relationship.Type)
                .To(target)
                .Property("key", relationship.Id)
                .Property("sourceId", relationship.SourcePersonId)
                .Property("targetId", relationship.TargetPersonId)
                .Property("type", relationship.Type)
                .Promise(t => t.Iterate());
        }

        // DELETE: api/relationship/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            await _g.E()
                .Has("key", id)
                .Drop()
                .Promise(t => t.Next());
        }
    }
}
