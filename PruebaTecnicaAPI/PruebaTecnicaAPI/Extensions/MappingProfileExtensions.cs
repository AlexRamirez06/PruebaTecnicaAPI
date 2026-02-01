using AutoMapper;
using PruebaTecnicaAPI.Models;
using PruebaTecnicaAPI.Entities.Entities;

namespace PruebaTecnicaAPI.Extensions
{
    public class MappingProfileExtensions: Profile
    {
        public MappingProfileExtensions()
        {
            CreateMap<Cliente, ClienteViewModel>().ReverseMap();
            CreateMap<Producto, ProductoViewModel>().ReverseMap();
            CreateMap<Orden, OrdenViewModel>().ReverseMap();
        }

    }
}
