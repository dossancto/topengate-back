# Email Sender Integrations Submodule

Este módulo contém toda a configuração de integração com e-mail.

## Email Sender Manager

O módulo `EmailSenderManager` é responsável por gerenciar o envio de e-mails através dos diferentes provedores.

Seu foco é adicionar resiliência ao processo de envio de e-mails: caso um provedor falhe, outro será usado para evitar que a mensagem se perca.

Também é possível usar um `shuffle` no momento do envio que irá aleatorizar qual provedor será usado. Isso é útil para distribuir o envio de e-mails entre diferentes provedores e pode ajudar a reduzir custos ao consumir mais de um Free Tier por vez.

É ideal que pelo menos dois provedores estejam configurados; caso o primeiro falhe, o segundo será usado.

## Registro de provedores de e-mail

A classe `EmailSendersModule` é responsável por registrar o módulo e configurar os serviços necessários.

Primeiramente, crie um diretório para sua integração dentro da pasta `Submodules/Integrations/EmailSenders/Integrations/`.

Adicione uma classe de `Provider` e outra de `Setup`.

- **Classe Provider**: A classe provider deve implementar as interfaces de envio de e-mail que a integração dá suporte; aqui ficam as regras.

- **Classe Setup**: Esta classe deve registrar o provider como um serviço e adicionar o tipo de integração ao módulo. Qualquer configuração de cliente que a integração necessite deve ser registrada aqui.

Agora registre o setup no método `AddEmailSenderIntegrations` dentro do módulo `EmailSendersModule`.

Assim, o manager será capaz de usar seu provedor!

## Como implementar novos e-mails

Cada parte da aplicação que necessite enviar e-mails deve criar uma interface com os valores que serão usados no envio do template.

Por exemplo, se precisarmos enviar um e-mail notificando uma compra, será necessária uma interface como:

```csharp
public interface IEmailNotifyPurchase
{
    Task SendAsync(string email, string name, string productName, int quantity, decimal price);
}
```

A classe do EmailSenderManager deve obrigatoriamente implementar essa interface e ser registrada como serviço padrão na classe `EmailSendersModule`.

Então, este novo template deve ser implementado nas integrações de e-mail. Não se preocupe: não precisa ser implementado em todos os provedores, pois o Manager fará a seleção dos provedores disponíveis. Você pode ir implementando com o tempo!


