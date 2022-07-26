using Dataplace.Core.Domain.Notifications;
using MediatR;

namespace Dataplace.Imersao.Core.Tests.Fixtures.FakeOjetcts
{
    //Tive que trazer lá da dataplace core por conta do cast do extension method no commit do handler, ainda bem que não tá selado a classe.
    //Se colocar mais uma interface com esses métodos lá que dá pra montar o mock sem depender dessa classe, eu acho.
    //Mas de certa forma pode precisar dela para validar mensagens durante os testes
    public class FakeDomainNotification : MediatorDomainNotificationHandler, INotificationHandler<DomainNotification>
    {
       
    }
}
