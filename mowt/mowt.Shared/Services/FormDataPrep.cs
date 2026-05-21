using mowt.ServiceHandler;
using mowt.Shared.Models.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace mowt.Shared.Services
{
    public class FormDataPrep
    {
        public ServiceResult<MultipartFormDataContent> GetFormData<T>(T dto, string? filePropertyName, IFormFile? file = null, List<ProductRelationshipDto>? relationships = null)
        {
            if (dto == null) return ServiceResult<MultipartFormDataContent>.Failure(
                new BadRequestException("Model cannot be null. Please provide the necessary Dto"));

            var formData = new MultipartFormDataContent();
            try
            {
                // Loop through all properties of the DTO
                foreach (var property in dto.GetType().GetProperties())
                {
                    // Skip the file property
                    if (property.PropertyType == typeof(IFormFile))
                        continue;

                    var value = property.GetValue(dto);
                    if (value != null)
                    {
                        var stringContent = new StringContent(value.ToString() ?? string.Empty);
                        formData.Add(stringContent, property.Name);
                    }
                }
                // Add relationships as JSON
                if (relationships != null && relationships.Count > 0)
                {
                    var relationshipsJson = JsonSerializer.Serialize(relationships);
                    formData.Add(new StringContent(relationshipsJson), "Relationships");
                }

                // Add the file property if it exists
                if (file != null)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    //Setting headers
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);

                    formData.Add(streamContent, filePropertyName ?? "ImageFile", file.FileName);
                }

                // Return success result with the form data
                return ServiceResult<MultipartFormDataContent>.Success(formData);
            }
            catch (Exception ex)
            {
                // Return failure result with the error message
                return ServiceResult<MultipartFormDataContent>.Failure(new Exception($"Error preparing form data: {ex.Message}"));
            }
        }
    }
}
