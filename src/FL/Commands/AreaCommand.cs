using System.Collections;
using System.Collections.Generic;
using MediatR;
using Microsoft.Framework.ConfigurationModel;
using PetaPoco;

namespace FL.Commands
{
    public class AreaCommand : IRequest<IEnumerable<Area>>
    {
         
    }

    public class AreaCommandHandler : IRequestHandler<AreaCommand,IEnumerable<Area>>
    {
        private readonly IConfiguration _configuration;

        public AreaCommandHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<Area> Handle(AreaCommand message)
        {
            var database = new PetaPoco.Database(_configuration.Get("ConnectionStrings:MainDatabase"));
            var areas = database.Query<Area>("SELECT * FROM Areas");
            return areas;
        }
    }

    public class Area
    {
        public int Id { get; set; }
        public string AreaName { get; set; }
    }

}