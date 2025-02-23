using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sitca.Models;
using Sitca.Models.DTOs;
using Sitca.Models.ViewModels;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IArchivoRepository : IRepository<Archivo>
    {
        Task<bool> SaveFileData(Archivo data);

        List<EnumValueDto> GetTypeFilesCompany();

        Task<List<Archivo>> GetList(ArchivoFilterVm data, ApplicationUser user, string role);

        Task<bool> DeleteFile(int data, ApplicationUser user, string role);

        Task<Result<Models.DTOs.UploadRequest>> ValidateAndCreateUploadRequest(
            IFormCollection form,
            ApplicationUser user
        );

        Task<Result<FileUploadResponse>> ProcessFileUpload(UploadRequest request);
    }
}
