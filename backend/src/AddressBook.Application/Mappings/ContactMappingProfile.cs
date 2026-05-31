using AddressBook.Application.DTOs;
using AddressBook.Domain.Entities;
using AutoMapper;

namespace AddressBook.Application.Mappings;

public class ContactMappingProfile : Profile
{
    public ContactMappingProfile()
    {
        CreateMap<Contact, ContactDto>();
        CreateMap<CreateContactRequest, Contact>();
        CreateMap<UpdateContactRequest, Contact>();
    }
}
