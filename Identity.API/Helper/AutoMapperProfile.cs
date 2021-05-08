using AutoMapper;
using Identity.API.DTO;
using Identity.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.API.Helper
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<User, UserDTO>().ReverseMap();
			CreateMap<User, UserLoginDTO>().ReverseMap();
		}
	}
}
