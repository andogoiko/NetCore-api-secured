
using apiSecurizada.Models;
using apiSecurizada.Models.DTO;

namespace methods
{

    public static class ClassConverter
    {
        public static User UserDTOToUser(UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return null;
            }

            return new User
            {
                Id = userDTO.Id,
                FirstName = userDTO.FirstName,
                LastName = userDTO.LastName,
                Username = userDTO.Username,
                Email = userDTO.Email,
                
            };
        }

        public static UserDTO UserToUserDTO(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,
            };
        }
    }
        
}
