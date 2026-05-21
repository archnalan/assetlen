using mowt.Shared.Models.Models.ViewModels;
using Refit;
using System.ComponentModel.DataAnnotations;

namespace mowt.Shared.Apicalls
{
    public interface IUserDocumentsAPI
    {
        [Get("/api/UserDocuments/GetMyCollection")]
        Task<IApiResponse<List<UserDocumentDto>>> GetMyCollection();

        [Get("/api/UserDocuments/IsInCollection")]
        Task<IApiResponse<bool>> IsInCollection([Query][Required] string productId);

        [Post("/api/UserDocuments/ToggleDocument")]
        Task<IApiResponse<UserDocumentDto>> ToggleDocument([Query][Required] string productId);
    }
}
