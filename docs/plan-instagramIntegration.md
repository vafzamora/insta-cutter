# Instagram Integration Plan for Insta-Cutter

## 🔐 Registro da Aplicação

### 1. Meta for Developers
1. Acesse [developers.facebook.com](https://developers.facebook.com)
2. Crie uma conta de desenvolvedor Meta
3. Clique em "My Apps" → "Create App"
4. Escolha "Consumer" como tipo de app
5. Preencha os detalhes da aplicação

### 2. Configuração do Instagram
```
App Dashboard → Add Product → Instagram Basic Display
```

**Configurações necessárias:**
- **Instagram App ID**: Será gerado automaticamente
- **Instagram App Secret**: Guarde com segurança
- **Valid OAuth Redirect URIs**: `https://localhost:5001/auth/callback`
- **Deauthorize Callback URL**: `https://yourdomain.com/deauth`

## 📋 APIs Necessárias

### Instagram Basic Display API
- **Propósito**: Autenticação e acesso básico
- **Funcionalidades**: Login, perfil básico, média do usuário

### Instagram Graph API (Business/Creator)
- **Propósito**: Publicação de conteúdo
- **Requisitos**: Conta Business ou Creator
- **Funcionalidades**: Publicar fotos, carrosséis, stories

## 🛠️ Implementação Técnica

### 1. Adicionar Dependências NuGet
```xml
<PackageReference Include="Microsoft.AspNetCore.WebUtilities" Version="2.2.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="System.Net.Http" Version="4.3.4" />
```

### 2. Classe de Configuração
```csharp
public class InstagramConfig
{
    public string AppId { get; set; } = "YOUR_APP_ID";
    public string AppSecret { get; set; } = "YOUR_APP_SECRET";
    public string RedirectUri { get; set; } = "https://localhost:5001/auth/callback";
    public string[] Scopes { get; set; } = { "user_profile", "user_media" };
}
```

### 3. Serviço de Autenticação
```csharp
public class InstagramAuthService
{
    private readonly InstagramConfig _config;
    private readonly HttpClient _httpClient;

    public InstagramAuthService(InstagramConfig config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    public string GetAuthorizationUrl()
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _config.AppId,
            ["redirect_uri"] = _config.RedirectUri,
            ["scope"] = string.Join(",", _config.Scopes),
            ["response_type"] = "code"
        };

        return "https://api.instagram.com/oauth/authorize?" + 
               string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
    }

    public async Task<string> ExchangeCodeForTokenAsync(string code)
    {
        var parameters = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _config.AppId),
            new KeyValuePair<string, string>("client_secret", _config.AppSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", _config.RedirectUri),
            new KeyValuePair<string, string>("code", code)
        });

        var response = await _httpClient.PostAsync("https://api.instagram.com/oauth/access_token", parameters);
        var content = await response.Content.ReadAsStringAsync();
        
        var tokenResponse = JsonConvert.DeserializeObject<dynamic>(content);
        return tokenResponse.access_token;
    }
}
```

### 4. Serviço de Publicação
```csharp
public class InstagramPublishService
{
    private readonly HttpClient _httpClient;
    private string _accessToken;

    public InstagramPublishService()
    {
        _httpClient = new HttpClient();
    }

    public void SetAccessToken(string accessToken)
    {
        _accessToken = accessToken;
    }

    public async Task<string> CreateMediaContainerAsync(string imageUrl, string caption)
    {
        var parameters = new Dictionary<string, string>
        {
            ["image_url"] = imageUrl,
            ["caption"] = caption,
            ["access_token"] = _accessToken
        };

        var url = $"https://graph.instagram.com/v18.0/USER_ID/media?" + 
                 string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        var response = await _httpClient.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        
        var result = JsonConvert.DeserializeObject<dynamic>(content);
        return result.id;
    }

    public async Task<bool> PublishMediaAsync(string creationId)
    {
        var parameters = new Dictionary<string, string>
        {
            ["creation_id"] = creationId,
            ["access_token"] = _accessToken
        };

        var url = $"https://graph.instagram.com/v18.0/USER_ID/media_publish?" + 
                 string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        var response = await _httpClient.PostAsync(url, null);
        return response.IsSuccessStatusCode;
    }

    // Para carrossel (múltiplas imagens)
    public async Task<string> CreateCarouselAsync(List<string> imageUrls, string caption)
    {
        var childrenIds = new List<string>();

        // Criar containers para cada imagem
        foreach (var imageUrl in imageUrls)
        {
            var childId = await CreateChildMediaAsync(imageUrl);
            childrenIds.Add(childId);
        }

        // Criar container do carrossel
        var parameters = new Dictionary<string, string>
        {
            ["media_type"] = "CAROUSEL",
            ["children"] = string.Join(",", childrenIds),
            ["caption"] = caption,
            ["access_token"] = _accessToken
        };

        var url = $"https://graph.instagram.com/v18.0/USER_ID/media?" + 
                 string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        var response = await _httpClient.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        
        var result = JsonConvert.DeserializeObject<dynamic>(content);
        return result.id;
    }

    private async Task<string> CreateChildMediaAsync(string imageUrl)
    {
        var parameters = new Dictionary<string, string>
        {
            ["image_url"] = imageUrl,
            ["is_carousel_item"] = "true",
            ["access_token"] = _accessToken
        };

        var url = $"https://graph.instagram.com/v18.0/USER_ID/media?" + 
                 string.Join("&", parameters.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

        var response = await _httpClient.PostAsync(url, null);
        var content = await response.Content.ReadAsStringAsync();
        
        var result = JsonConvert.DeserializeObject<dynamic>(content);
        return result.id;
    }
}
```

## 🎯 Integração no FormMain

### Novo Menu Item
```csharp
// No FormMain.Designer.cs
private ToolStripMenuItem publishToInstagramToolStripMenuItem;

// Adicionar ao menu File
this.publishToInstagramToolStripMenuItem = new ToolStripMenuItem();
this.publishToInstagramToolStripMenuItem.Name = "publishToInstagramToolStripMenuItem";
this.publishToInstagramToolStripMenuItem.Text = "Publish to Instagram";
this.publishToInstagramToolStripMenuItem.Click += new EventHandler(this.publishToInstagramToolStripMenuItem_Click);
```

### Event Handler
```csharp
private async void publishToInstagramToolStripMenuItem_Click(object sender, EventArgs e)
{
    if (!hasImageLoaded || pictureBox1.Image == null)
    {
        MessageBox.Show("Please load an image first.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    try
    {
        // 1. Salvar imagens temporariamente
        var tempPath = Path.GetTempPath();
        var leftImagePath = Path.Combine(tempPath, "insta_left.jpg");
        var rightImagePath = Path.Combine(tempPath, "insta_right.jpg");

        // Salvar as imagens recortadas
        SaveCroppedImages(leftImagePath, rightImagePath);

        // 2. Upload para servidor temporário (você precisará implementar)
        var leftImageUrl = await UploadToTempServerAsync(leftImagePath);
        var rightImageUrl = await UploadToTempServerAsync(rightImagePath);

        // 3. Criar carrossel no Instagram
        var publishService = new InstagramPublishService();
        publishService.SetAccessToken(GetStoredAccessToken());

        var imageUrls = new List<string> { leftImageUrl, rightImageUrl };
        var caption = "Created with Insta-Cutter 📸 #instacutter";

        var creationId = await publishService.CreateCarouselAsync(imageUrls, caption);
        var success = await publishService.PublishMediaAsync(creationId);

        if (success)
        {
            MessageBox.Show("Successfully published to Instagram!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Failed to publish to Instagram.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Limpar arquivos temporários
        File.Delete(leftImagePath);
        File.Delete(rightImagePath);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error publishing to Instagram: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

## 🌐 Servidor Temporário para Imagens

O Instagram requer URLs públicas. Você precisará de:

### Opção 1: Azure Blob Storage
```csharp
public async Task<string> UploadToAzureBlobAsync(string filePath)
{
    // Implementar upload para Azure Blob Storage
    // Retornar URL pública
}
```

### Opção 2: AWS S3
```csharp
public async Task<string> UploadToS3Async(string filePath)
{
    // Implementar upload para AWS S3
    // Retornar URL pública
}
```

### Opção 3: Servidor Próprio
Criar uma API simples para upload temporário.

## 📋 Checklist de Implementação

### Próximos Passos:

1. ✅ **Registrar app no Meta for Developers**
2. ✅ **Implementar autenticação OAuth**
3. ✅ **Criar serviço de publicação**
4. ✅ **Implementar upload de imagens temporário**
5. ✅ **Adicionar UI para conectar Instagram**
6. ✅ **Testar com conta Business/Creator**
7. ✅ **Implementar tratamento de erros**
8. ✅ **Adicionar progress indicators**

### Considerações Importantes:

- **Conta Business**: Instagram Graph API requer conta Business ou Creator
- **Rate Limits**: Instagram tem limites de publicação por hora
- **Approval**: Algumas funcionalidades podem precisar de aprovação da Meta
- **Webhooks**: Para notificações de status de publicação

## 🚀 Arquitetura Proposta

### Novas Classes a Criar:
1. **InstagramConfig**: Configurações da API
2. **InstagramAuthService**: Serviço de autenticação OAuth
3. **InstagramPublishService**: Serviço de publicação
4. **ImageUploadService**: Upload temporário de imagens
5. **InstagramDialog**: Interface para conectar e configurar Instagram

### Fluxo de Trabalho:
1. **Primeira vez**: Usuário conecta conta Instagram via OAuth
2. **Uso normal**: Carregar imagem → Ajustar seleção → Publicar diretamente
3. **Background**: Upload temporário → Criação de carrossel → Publicação

### Melhorias na UI:
- Botão "Connect Instagram" no menu
- Status de conexão na barra de status
- Progress bar durante publicação
- Preview do post antes de publicar