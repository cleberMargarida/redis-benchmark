using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ingestion.Api.Dto
{
    /// <summary>
    /// Represents the data of a signal received from a device. Here is a breakdown of the class:
    /// </summary>
    public class SignalDataDto
    {
        private Guid? _hashCode;

        /// <summary>
        /// Gets or sets the unique identifier of the device sending the signal.
        /// </summary>
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the unique identifier of the object associated with the signal.
        /// </summary>
        [JsonProperty("id")]
        public string ObjectId { get; set; } = default!;

        /// <summary>
        /// Gets or sets the Unix timestamp (in milliseconds) indicating when the signal was generated.
        /// </summary>
        [JsonProperty("timestamp")]
        public long TimeStamp { get; set; } = default!;

        /// <summary>
        /// Gets or sets the value associated with the signal.
        /// </summary>
        [JsonProperty("value")]
        public string? Value { get; set; } = default!;

        /// <summary>
        /// Gets or sets an array of key-value pairs associated with the gps signal.
        /// </summary>
        [JsonProperty("keyValue")]
        public KeyValuePair<long, double>[] KeyValue { get; set; } = default!;

        /// <summary>
        /// Gets the UTC date and time when the signal was generated.
        /// </summary>
        [JsonIgnore]
        public DateTime DateTimeUTC => DateTime.UnixEpoch.AddMilliseconds(TimeStamp);

        /// <summary>
        /// Gets or sets the vehicle number associated with the signal.
        /// </summary>
        [JsonIgnore]
        public string VehicleNumber { get; set; } = default!;

        /// <summary>
        /// Gets or sets the speed associated with the signal.
        /// </summary>
        [JsonIgnore]
        public float? Speed { get; set; } = default!;

        /// <summary>
        /// Gets or sets the unit text associated with the signal.
        /// </summary>
        [JsonIgnore]
        public string? UnitText { get; set; } = default!;

        /// <summary>
        /// Gets the hash code for the signal data.
        /// </summary>
        [JsonIgnore]
        public Guid HashCode => _hashCode ??= Guid.Parse(GetMd5HashCode());

        /// <summary>
        /// Gets or sets the tenant identifier for the signal data.
        /// </summary>
        [JsonIgnore]
        public int TenantId { get; set; } = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalDataDto"/> class.
        /// </summary>
        public SignalDataDto()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalDataDto"/> class with the specified device ID, object ID, timestamp, and value.
        /// </summary>
        /// <param name="deviceId">The unique identifier of the device sending the signal.</param>
        /// <param name="objectId">The unique identifier of the object associated with the signal.</param>
        /// <param name="timeStamp">The Unix timestamp (in milliseconds) indicating when the signal was generated.</param>
        /// <param name="value">The value associated with the signal.</param>
        public SignalDataDto(string deviceId, string objectId, long timeStamp, string? value)
        {
            DeviceId = deviceId;
            ObjectId = objectId;
            TimeStamp = timeStamp;
            Value = value;
        }

        private string GetMd5HashCode()
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var input = ObjectId + TimeStamp + DeviceId + (Value?.ToString() ?? JsonConvert.SerializeObject(KeyValue) ?? "");
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}

