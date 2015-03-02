using AutoMapper;
using StephenZeng.MvcTemplate.Common.Entities;
using StephenZeng.MvcTemplate.Web.Models;

namespace StephenZeng.MvcTemplate.Web
{
    public class AutoMapperConfig
    {
        public static void Config()
        {
            Mapper.CreateMap<User, UserViewModel>()
                .ForMember(d => d.Roles, o => o.Ignore());

            Mapper.CreateMap<User, EditUserViewModel>();

            Mapper.CreateMap<EditUserViewModel, User>();

            Mapper.CreateMap<User, ProfileViewModel>();

            Mapper.CreateMap<User, ClientProfileViewModel>();

            Mapper.CreateMap<User, EditProfileViewModel>();

            Mapper.CreateMap<RegisterViewModel, User>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.Email));
        }
    }
}