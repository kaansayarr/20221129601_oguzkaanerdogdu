﻿using AutoMapper;
using Internet_1.Models;
using Internet_1.ViewModels;

namespace Internet_1.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<Lesson, LessonModel>().ReverseMap();
            CreateMap<LessonVideo, LessonVideoModel>().ReverseMap();
            CreateMap<LessonInstructor, LessonInstructorModel>().ReverseMap();
            CreateMap<Product, ProductModel>().ReverseMap();
            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<AppUser, UserModel>().ReverseMap();
            CreateMap<AppUser, RegisterModel>().ReverseMap();
            CreateMap<Todo, TodoModel>().ReverseMap();
        }
    }
}
