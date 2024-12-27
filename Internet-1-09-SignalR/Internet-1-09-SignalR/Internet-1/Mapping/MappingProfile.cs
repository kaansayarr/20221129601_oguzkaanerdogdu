using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Lesson, LessonModel>()
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore());

        CreateMap<LessonModel, Lesson>();

        CreateMap<LessonVideo, LessonVideoModel>();
        CreateMap<LessonVideoModel, LessonVideo>();
    }
}