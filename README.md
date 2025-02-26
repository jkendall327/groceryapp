# GroceryApp

GroceryApp is a modern full-stack application designed to streamline grocery shopping and receipt management. It includes a robust backend built with ASP.NET Core and a dynamic frontend built with Blazor.

## Features

- **Receipt Processing:** Upload and manage grocery receipts with automatic scanning and analysis.
- **Product Tracking:** View purchased items and detailed product information.
- **Real-time Analytics:** Analyze receipts using integrated computer vision and LLM services.
- **Cosmos DB Integration:** Store and manage product data efficiently using Azure Cosmos DB.
- **Blob Storage:** Securely upload images and handle file storage using Azure Blob Storage.
- **User Authentication:** Implement secure user authentication with JWT.

## Project Structure

- **GroceryApp.Backend:** Contains controllers, services, models, and configuration for the ASP.NET Core backend.
- **GroceryApp.Frontend:** Hosts Blazor components and pages for the user interface.
- **GroceryApp.Backend.Tests:** Houses unit and integration tests for backend functionality.

## Getting Started

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/yourusername/GroceryApp.git
   cd GroceryApp
   ```

2. **Build the Project:**
   ```bash
   dotnet build
   ```

3. **Run the Backend:**
   ```bash
   dotnet run --project GroceryApp.Backend
   ```

4. **Run the Frontend:**
   From within the GroceryApp.Frontend directory:
   ```bash
   dotnet run
   ```

5. **Running Tests:**
   ```bash
   dotnet test GroceryApp.Backend.Tests
   ```

## Configuration

Remember to update configuration files with your Azure Cosmos DB connection strings, Blob Storage credentials, and any authentication settings as needed.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch: `git checkout -b feature/my-feature`
3. Commit your changes: `git commit -am 'Add new feature'`
4. Push the branch: `git push origin feature/my-feature`
5. Open a pull request.

## License

This project is licensed under the MIT License.
