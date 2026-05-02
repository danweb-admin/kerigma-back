using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using QRCoder;
using RccManager.Domain.Entities;
using RccManager.Domain.Helpers;

namespace RccManager.Service.Helper;

public static class Utils
{
    public static DateTime formatDate(string date)
    {

        date = date.Replace("/", "");
        var year = int.Parse(date.Substring(4));
        var day = int.Parse(date.Substring(0, 2));
        var month = int.Parse(date.Substring(2, 2));

        return new DateTime(year, month, day);
    }

    public static DateTime formatDate2(string date)
    {
        string[] split;
        int year, day, month;

        if (date.Contains("-"))
        {
            split = date.Split("-");
            year = int.Parse(split[0]);
            day = int.Parse(split[2]);
            month = int.Parse(split[1]);

            return new DateTime(year, month, day);
        }

        if (date.Contains("/"))
        {
            split = date.Split("/");
            year = int.Parse(split[2]);
            day = int.Parse(split[0]);
            month = int.Parse(split[1]);

            return new DateTime(year, month, day);
        }

        day = int.Parse(date.Substring(0, 2));
        month = int.Parse(date.Substring(2, 2));
        year = int.Parse(date.Substring(4));

        return new DateTime(year, month, day);
    }

    public static DateTime formaTime(string time)
    {
        time = time.Replace(":", "");
        var hours = int.Parse(time.Substring(0, 2));
        var minutes = int.Parse(time.Substring(2));

        var now = Helpers.DateTimeNow();

        return new DateTime(now.Year, now.Month, now.Day, hours, minutes, 0);
    }

    public static DateTime formatDateTime(string dateString)
    {
        string format = "dd/MM/yyyy HH:mm:ss";
        DateTime dateTime;

        if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            return dateTime; 
        else
            return DateTime.Now;

    }



    public static string Encrypt(string text)
    {
        string key = Environment.GetEnvironmentVariable("CryptKey");
        string iv = Environment.GetEnvironmentVariable("CryptIV");

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(text);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }

    }

    public static string Decrypt(string text)
    {
        string key = Environment.GetEnvironmentVariable("CryptKey");
        string iv = Environment.GetEnvironmentVariable("CryptIV");

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    public static string GerarQrCodePNG(string codigoInscricao)
    {
        var pasta = Path.Combine(Directory.GetCurrentDirectory(), "qrcodes");

        var urlCheckin = $"https://backend.auxiliadoraonline.com.br/api/v1/eventos/{codigoInscricao}/checkin";

        if (!Directory.Exists(pasta))
            Directory.CreateDirectory(pasta);

        var caminhoArquivo = Path.Combine(pasta, $"{codigoInscricao}.png");

        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(urlCheckin, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);

        byte[] bytes = qrCode.GetGraphic(20);

        File.WriteAllBytes(caminhoArquivo, bytes);

        Console.WriteLine(Directory.GetCurrentDirectory());

        return $"/qrcodes/{codigoInscricao}.png";
    }

    public static string SomenteNumeros(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return string.Empty;

        return Regex.Replace(texto, @"\D", "");
    }
}

