using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ttc.DataEntities.Core;
using Ttc.Model.Core;

namespace Ttc.WebApi.Controllers;

[Authorize]
[Route("api/upload")]
public class UploadController
{
    private readonly TtcSettings _settings;
    private readonly ITtcDbContext _context;

    public UploadController(TtcSettings settings, ITtcDbContext context)
    {
        _settings = settings;
        _context = context;
    }

    [HttpPost]
    [Route("Image")]
    public async Task UploadImage([FromBody] UploadImageDto data)
    {
        var file = GetServerPlayerImageFile(data.Type, data.DataId);
        if (file.Exists)
        {
            var backupFile = GetServerImagePath(ImageFolder.Backup) + "\\";
            backupFile += data.Type.Replace("-", "_") + "_" + data.DataId + "_" + Path.GetRandomFileName() + ".png";
            File.Move(file.FullName, backupFile);
        }

        if (data.Type is "player-photo" or "player-avatar")
        {
            var player = await _context.Players.SingleOrDefaultAsync(x => x.Id == data.DataId);
            if (player != null)
            {
                player.ImageVersion++;
                await _context.SaveChangesAsync();
            }
        }

        string base64String = data.Image.Substring(22);
        byte[] bytes = Convert.FromBase64String(base64String);
        await File.WriteAllBytesAsync(file.FullName, bytes);
    }

    [HttpPost]
    public async Task<IResult> UploadTempFile(IFormFile file, [FromForm] string uploadType, [FromForm] int uploadTypeId)
    {
        string fullPath = GetServerImagePath(ImageFolder.Temp);

        try
        {
            var originalFileExtension = Path.GetExtension(file.FileName);
            var tempFilePath = Path.Combine(fullPath, uploadType + "_" + uploadTypeId + "_" + Path.GetRandomFileName());
            var tempFilePathWithExtension = Path.ChangeExtension(tempFilePath, originalFileExtension);

            // Save the file to the temporary folder
            await using (var stream = new FileStream(tempFilePathWithExtension, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (uploadType.Equals("match", StringComparison.OrdinalIgnoreCase))
            {
                var matchFolderPath = GetServerImagePath(ImageFolder.Match);
                Directory.CreateDirectory(matchFolderPath);

                var matchFilePath = Path.Combine(matchFolderPath, Path.GetFileName(tempFilePathWithExtension));
                File.Move(tempFilePathWithExtension, matchFilePath);

                return Results.Ok(new { fileName = $"/img/matches/{Path.GetFileName(matchFilePath)}" });
            }

            string publicFileName = $"/img/temp/{Path.GetFileName(tempFilePathWithExtension)}";
            return Results.Ok(new { fileName = publicFileName });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    #region Private FileSystem stuff
    private FileInfo GetServerPlayerImageFile(string type, int playerId)
    {
        var path = GetServerImagePath(ImageFolder.Players);
        if (type == "player-photo")
        {
            path = Path.Combine(path, playerId + ".png");
        }
        else
        {
            path = Path.Combine(path, playerId + "_avatar.png");
        }

        return new FileInfo(path);
    }

    private string GetServerImagePath(ImageFolder folder)
    {
        var fullPath = GetServerImagePath(_settings.PublicImageFolder, folder);
        return fullPath;
    }

    private static string GetServerImagePath(string root, ImageFolder folder)
    {
        string fullPath;
        switch (folder)
        {
            case ImageFolder.Temp:
                fullPath = Path.Combine(root, "temp");
                break;

            case ImageFolder.Backup:
                fullPath = Path.Combine(root, "backup");
                break;

            case ImageFolder.Match:
                fullPath = Path.Combine(root, "matches");
                break;

            case ImageFolder.Players:
            default:
                fullPath = Path.Combine(root, "players");
                break;
        }

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        return fullPath;
    }

    private enum ImageFolder
    {
        Temp,
        Players,
        Backup,
        Match
    }
    #endregion
}

public class UploadImageDto
{
    /// <summary>
    /// Base64 encoded
    /// </summary>
    public string Image { get; set; }
    public int DataId { get; set; }
    public string Type { get; set; }

    public override string ToString() => $"Image: {Image}, Id: {DataId}";
}
