
## Developer PowerShell Terminali

1.)Terminal açildiktan sonra => cd MentorApp komutu ile proje icerisinde MentorApp.csproj.vspscc dosyasinin bulundugu dizine geçilir.

2.)Asagidaki komutlar calistirilarak Azure platformunda organizasyona kayit olmaniz gerekir.
dotnet user-secrets init
dotnet user-secrets set "AzureAd:ClientId" "fed0f06e-b43a-4bbe-ae7a-40f36f8e17f4"
dotnet user-secrets set "AzureAd:ClientSecret" "88iA~_VCCT6I8~6cy_vhWpJtl24aO_lw~3"
