using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ReadJitsiRecordings.service
{
    public interface IStorageService
    {
        Task<bool> UploadFileAsync(string directory, string path, string name);
    }
}
