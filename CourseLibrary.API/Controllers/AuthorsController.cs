﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CourseLibrary.API.Services;
using CourseLibrary.API.Models;
using AutoMapper;
using CourseLibrary.API.ResourceParameters;
using CourseLibrary.API.Entities;
using Microsoft.AspNetCore.JsonPatch;
using CourseLibrary.API.Helpers;
using System.Text.Json;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors")]

    public class AuthorsController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;
        private readonly IPropertyMappingService _propertyMappingService;

        public AuthorsController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper, IPropertyMappingService propertyMappingService)
        {
            _courseLibraryRepository = courseLibraryRepository ??
                throw new ArgumentNullException(nameof(courseLibraryRepository));

            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));

            _propertyMappingService = propertyMappingService ??
                throw new ArgumentNullException(nameof(propertyMappingService));
        }

        [HttpGet(Name = "GetAuthors")]
        [HttpHead]
        public IActionResult GetAuthors([FromQuery] AuthorsResourceParameters authorsResourceParameters)
        {

            if(!_propertyMappingService.ValidMappingExistsFor<AuthorDto, Author>(authorsResourceParameters.OrderBy))
            {
                return BadRequest();
            }
            var authorsFromRepo = _courseLibraryRepository.GetAuthors(authorsResourceParameters);

            var previousPageLink = authorsFromRepo.HasPrevious ?
                CreateAuthorsResourceUri(authorsResourceParameters,
                ResourceUriType.PreviousPage) : null;

            var nextPageLink = authorsFromRepo.HasNext ?
               CreateAuthorsResourceUri(authorsResourceParameters,
               ResourceUriType.NextPage) : null;

            var paginationMetaData = new
            {
                totalCount = authorsFromRepo.TotalCount,
                pageSize = authorsFromRepo.PageSize,
                currentPage = authorsFromRepo.Currentpage,
                totalPages = authorsFromRepo.TotalPages,
                previousPageLink,
                nextPageLink

            };

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetaData));

            return Ok(_mapper.Map<IEnumerable<AuthorDto>>(authorsFromRepo).ShapeData(authorsResourceParameters.Fields));
        }

        [HttpGet("{authorId}", Name = "GetAuthor" )]
        public IActionResult GetAuthor(Guid authorId)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }  

            return  Ok(_mapper.Map<AuthorDto>(authorFromRepo));
        }

        [HttpPost]
        public ActionResult<AuthorDto> CreateAuthor(AuthorForCreationDto author)
        {
            var authorEntity = _mapper.Map<Author>(author);
            _courseLibraryRepository.AddAuthor(authorEntity);
            _courseLibraryRepository.Save();

            var authorToReturn = _mapper.Map<AuthorDto>(authorEntity);

            return CreatedAtRoute(  "GetAuthor",
                                    new { authorId = authorToReturn.Id },
                                    authorToReturn);
        }

        [HttpOptions]
        public IActionResult GetAuthorsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        [HttpDelete("{authorId}")]
        public ActionResult DeleteAuthor(Guid authorId)
        {
            var authorFromRepo = _courseLibraryRepository.GetAuthor(authorId);

            if (authorFromRepo == null)
            {
                return NotFound();
            }

            _courseLibraryRepository.DeleteAuthor(authorFromRepo);

            _courseLibraryRepository.Save();

            return NoContent();
        }

        private string CreateAuthorsResourceUri(AuthorsResourceParameters authorsResourceParameters, ResourceUriType type)
        {
            switch(type)
            {
                case ResourceUriType.PreviousPage:
                    return Url.Link("GetAuthors",
                    new
                    {
                        authors = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    }); ;
                case ResourceUriType.NextPage:
                    return Url.Link("GetAuthors",
                    new 
                    {
                        authors = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber - 1,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
                default:
                    return Url.Link("GetAuthors",
                    new
                    {
                        authors = authorsResourceParameters.Fields,
                        orderBy = authorsResourceParameters.OrderBy,
                        pageNumber = authorsResourceParameters.PageNumber,
                        pageSize = authorsResourceParameters.PageSize,
                        mainCategory = authorsResourceParameters.MainCategory,
                        searchQuery = authorsResourceParameters.SearchQuery
                    });
            }
        }
    }
}