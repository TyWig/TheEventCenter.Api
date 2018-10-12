using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheEventCenter.Api.Db.Models;

namespace TheEventCenter.Api.ViewModels.Mappings
{
	public class RegistrationViewModelToAppUser : Profile
	{
		public RegistrationViewModelToAppUser()
		{
			CreateMap<RegistrationViewModel, AppUser>().ForMember(au => au.UserName, map => map.MapFrom(vm => vm.Email));
		}
	}
}
