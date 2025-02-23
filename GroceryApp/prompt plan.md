Below is a comprehensive blueprint for building the Grocery Expiration Tracker project. This blueprint starts with a high-level architecture, then drills down into iterative, small, well-integrated chunks. Finally, you’ll find a series of prompts (each in its own markdown “code” block tagged as text) that you can feed into a code-generation LLM. Each prompt builds on the previous work so that by the end, everything is wired together with no orphaned pieces.

---

## 1. High-Level Blueprint

### **A. Architecture & Core Components**

- **Frontend (Blazor Server)**
  - **Receipt Upload Page:** A page/component where users upload receipt images.
  - **Expiration Calendar:** A view that lists expiring items, with past dates greyed out and a bulk “mark as used” feature.
  - **Shopping History:** A separate page that displays past purchases and offers export functionality.
  - **Authentication:** OAuth integration (e.g., Microsoft, Google) so users can sign in securely.

- **Backend**
  - **Receipt Processing API:**
    - **OCR Integration:** Call Azure Computer Vision to extract text from uploaded images.
    - **LLM Integration:** Pass the OCR text to an OpenAI API to get structured JSON output (including nutritional info, shelf life, etc.).
    - **Fallbacks:** If OCR/LLM fails or returns low confidence, prompt the user for additional input.
  - **Data Persistence:**
    - **Cosmos DB:** Store shopping history and user details (no PII).
    - **Blob Storage (Temporary):** Used for storing images until processing is complete.
  - **Logging & Error Handling:**
    - **Azure Application Insights:** Monitor API calls and log errors.
    - **Error Fallbacks:** Graceful error messages and fallback to manual entry when needed.

- **Supporting Services**
  - **Azure Resources Setup:** Cosmos DB, Blob Storage, Computer Vision API, and Application Insights.
  - **Testing Infrastructure:** Unit tests (for OCR, LLM output validation, database operations) and integration tests (end-to-end receipt processing, authentication flow).

---

## 2. Iterative Breakdown into Chunks

### **Phase 1: Project Initialization and Environment Setup**
- **Step 1:** Create the project repository with two solutions:
  - A Blazor Server project for the frontend.
  - A backend API project (using either ASP.NET Core for tighter integration with Blazor or Rust if preferred).
- **Step 2:** Set up configuration files and dependency management.
- **Step 3:** Create stubs for Azure services (Cosmos DB, Blob Storage, Computer Vision API).

### **Phase 2: Implementing the Receipt Processing Pipeline**
- **Step 4:** Develop the file upload component in Blazor.
- **Step 5:** In the backend, create an endpoint to receive the receipt image.
- **Step 6:** Integrate a service that sends the image to Azure Computer Vision for OCR.
- **Step 7:** Process the OCR results and call an LLM service (via OpenAI API) to extract product details.
- **Step 8:** Handle low-confidence cases with a fallback prompt for user input.
- **Step 9:** Ensure temporary images are deleted after processing.

### **Phase 3: Building the Frontend Components**
- **Step 10:** Create a calendar view in Blazor that displays expiring items.
- **Step 11:** Build the shopping history page to list past receipts and purchases.
- **Step 12:** Add export functionality (JSON/CSV) for shopping history.
- **Step 13:** Integrate OAuth authentication into the Blazor app and secure the endpoints.

### **Phase 4: Integrating Logging, Error Handling, and Testing**
- **Step 14:** Integrate Azure Application Insights for logging.
- **Step 15:** Implement error handling (for OCR, LLM calls, and API rate limits) with user-friendly prompts.
- **Step 16:** Develop unit tests and integration tests for each major component.

### **Phase 5: Final Integration and Wiring Everything Together**
- **Step 17:** Wire all modules together so that:
  - The receipt upload component triggers the entire pipeline.
  - Processed data updates the expiration calendar and shopping history.
  - Authentication and logging are active across all endpoints.
- **Step 18:** Conduct full end-to-end tests and refine based on feedback.

---

## 3. Refining the Iterative Steps into Small, Incremental Prompts

Each prompt below is designed to be fed into a code-generation LLM. Every prompt ends with wiring that module into the previous parts to ensure complete integration.

---

### **Prompt 1: Project Initialization and Environment Setup**

```text
Prompt 1: 
You are tasked with creating the initial project structure for the Grocery Expiration Tracker. Please generate:
1. A solution with two projects:
   - A Blazor Server project for the frontend.
   - A backend API project (preferably using ASP.NET Core for tight integration with Blazor).
2. Basic configuration files, including dependency management (NuGet packages for Azure, logging, authentication, etc.).
3. Stubs for services that will later interface with Azure Cosmos DB, Blob Storage, and Azure Computer Vision.
Ensure that the projects are structured so that the backend API is callable from the Blazor frontend. End with a note on how these services will later be integrated.
```

---

### **Prompt 2: Azure Integration Stubs**

```text
Prompt 2:
Now that the basic project structure is in place, please generate code for Azure integration stubs in the backend project. This should include:
1. A stub for connecting to Azure Blob Storage (for temporarily holding uploaded images).
2. A stub for interacting with Azure Cosmos DB (for storing shopping history).
3. A stub for connecting to Azure Computer Vision API (to perform OCR on uploaded receipt images).
Include placeholder methods that simulate the behavior of these services, and ensure they are wired into a service layer that can be called from the receipt processing endpoint.
```

---

### **Prompt 3: Receipt Upload and Processing Endpoint**

```text
Prompt 3:
Develop the receipt upload and processing functionality. In this prompt, generate:
1. A Blazor component that allows the user to upload a receipt image.
2. An API endpoint in the backend that receives the uploaded image and temporarily stores it using the Blob Storage stub.
3. Code that calls the Azure Computer Vision stub to perform OCR on the image.
4. Basic error handling to manage failures in the OCR call.
Ensure that the Blazor component calls the backend endpoint correctly and that the result from OCR is returned to the UI for further processing.
```

---

### **Prompt 4: LLM Integration for Product Extraction**

```text
Prompt 4:
Extend the receipt processing functionality by integrating an LLM service. Please generate:
1. A service class in the backend that takes OCR text as input and calls the OpenAI API (or similar LLM service) to extract structured product information in JSON format.
2. Code that processes the JSON output to extract fields like nutritional info, shelf life, food category, unit, and quantity.
3. A mechanism to detect low confidence in the returned data and trigger a fallback prompt for user input.
Integrate this LLM service into the existing receipt processing endpoint so that after OCR, the text is passed to this new service and the structured data is returned to the frontend.
```

---

### **Prompt 5: Building the Expiration Calendar UI**

```text
Prompt 5:
Now, create the frontend component for the Expiration Calendar. Generate:
1. A Blazor component that displays a calendar view with dates for the current week.
2. Code to list expiring food items in descending order alongside the dates, with past days greyed out.
3. A bulk edit feature that lets users mark items as "used" (and hence remove or hide them).
Make sure this component fetches data from the backend (e.g., the processed receipts stored in Cosmos DB) and integrates with the receipt processing flow.
```

---

### **Prompt 6: Developing the Shopping History Page**

```text
Prompt 6:
Generate the shopping history functionality in the Blazor project. This should include:
1. A Blazor page that displays a list of past purchases retrieved from the Cosmos DB stub.
2. Code to group purchases by user and include fields such as product name, expiration date, nutritional info, and used status.
3. An export feature that allows the user to download the shopping history as either JSON or CSV.
Ensure that this page is integrated with the backend service that stores processed receipt data and that the data remains consistent across the project.
```

---

### **Prompt 7: OAuth Authentication Integration**

```text
Prompt 7:
Add authentication support to the application. Please generate:
1. Code to integrate OAuth providers (e.g., Microsoft and Google) in the Blazor project.
2. Secure the backend API endpoints so that only authenticated users can call them.
3. A mechanism to ensure that user-specific data (like shopping history) is isolated per user.
Ensure that the authentication system is seamlessly integrated with the receipt upload, processing, and data display features.
```

---

### **Prompt 8: Logging, Error Handling, and Final Integration**

```text
Prompt 8:
Finalize the project by integrating logging and robust error handling. Please generate:
1. Code to integrate Azure Application Insights into both the backend and Blazor frontend for logging and monitoring.
2. Detailed error handling for each service call (OCR, LLM, Blob Storage, Cosmos DB), including user-friendly error messages and fallback mechanisms.
3. Unit and integration test scaffolding for key functionalities (receipt processing, calendar display, authentication).
4. Final wiring that connects the receipt upload, processing pipeline, expiration calendar, and shopping history so that the full end-to-end flow works.
Ensure that all components now work together seamlessly with no orphaned code, and include comments on how to run tests and monitor logs.
```

---

By following these prompts in sequence, you will iteratively build out the entire Grocery Expiration Tracker project. Each step is small and incremental, ensuring that no big jumps in complexity occur and that every new piece is integrated with the previous work. This approach helps maintain best practices while steadily moving the project forward.
