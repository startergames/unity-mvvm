using System.Threading.Tasks;

public interface IMessageBoxService {
    Task<bool> Show(string title,
                    string message,
                    string ok, 
                    string cancel,
                    System.Action okAction,
                    System.Action cancelAction = null);
}