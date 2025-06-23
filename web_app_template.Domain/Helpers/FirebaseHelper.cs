using Firebase.Storage;
using Microsoft.Extensions.Configuration;

namespace web_app_template.Domain.Helpers
{
    public static class FirebaseHelper
    {
        private static IConfiguration _config;

        public static void Initialize(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Uploads a file to Firebase Storage under the specified directory.
        /// </summary>
        /// <remarks>
        /// The method uploads the file to a Firebase Storage bucket specified in the configuration. The file is stored in the given directory with the provided <paramref name="fileName"/>. The operation supports cancellation through a <see cref="CancellationToken"/>.
        ///  The configuration must be located under the "Firebase:StorageBucket" key.
        /// </remarks>
        /// <param name="stream">The <see cref="Stream"/> containing the file data to upload. Must not be null.</param>
        /// <param name="fileName">The name of the file to be uploaded. This will be used as the file's identifier in storage. Must not be null or empty.</param>
        /// <param name="directory">The directory in Firebase Storage where the file will be uploaded. Must not be null or empty.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the download URL of the uploaded file.</returns>
        public static async Task<string> UploadFile(Stream stream, string fileName, string directory)
        {
            var cancellation = new CancellationTokenSource();
            var task = new FirebaseStorage(
                _config["Firebase:StorageBucket"],
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult<string>(null),
                    ThrowOnCancel = true
                })
                .Child(directory)
                .Child(fileName)
                .PutAsync(stream, cancellation.Token);

            return await task;
        }

        /// <summary>
        /// Removes a file from Firebase Storage under the specified directory.
        /// </summary>
        /// <remarks>
        /// The method deletes the file from a Firebase Storage bucket specified in the configuration. The file is identified by the given directory and <paramref name="fileName"/>.
        /// The configuration must be located under the "Firebase:StorageBucket" key.
        /// </remarks>
        /// <param name="fileName">The name of the file to be removed. Must not be null or empty.</param>
        /// <param name="directory">The directory in Firebase Storage where the file is located. Must not be null or empty.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task RemoveFile(string fileName, string directory)
        {
            await new FirebaseStorage(
                _config["Firebase:StorageBucket"],
                new FirebaseStorageOptions
                {
                    ThrowOnCancel = true
                })
                .Child(directory)
                .Child(fileName)
                .DeleteAsync();
        }
    }
}
