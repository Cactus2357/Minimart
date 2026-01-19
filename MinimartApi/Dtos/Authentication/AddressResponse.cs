using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Authentication {
    public class AddressResponse {
        public string ReceiverName { get; set; }
        public string Phone { get; set; }
        public string AddressLine { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
