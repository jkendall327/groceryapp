# Grocery Expiration Tracker - TODO Checklist

## Phase 1: Project Initialization and Environment Setup
- [ ] **Repository Setup:**
  - [ ] Create a project repository.
  - [ ] Initialize two projects/solutions:
    - [ ] Blazor Server project (frontend).
    - [ ] Backend API project (preferably ASP.NET Core).
- [ ] **Configuration:**
  - [ ] Set up configuration files (appsettings, launchSettings, etc.).
  - [ ] Configure dependency management (install necessary NuGet packages for Azure, logging, authentication, etc.).
- [ ] **Azure Service Stubs:**
  - [ ] Create a stub for Azure Cosmos DB integration.
  - [ ] Create a stub for Azure Blob Storage integration.
  - [ ] Create a stub for Azure Computer Vision integration.

## Phase 2: Receipt Processing Pipeline Implementation
- [ ] **File Upload Component:**
  - [ ] Develop a Blazor component for users to upload receipt images.
- [ ] **Backend Endpoint:**
  - [ ] Create an API endpoint to receive uploaded images.
  - [ ] Wire the Blazor component to call this endpoint.
- [ ] **Temporary Image Storage:**
  - [ ] Integrate Blob Storage stub to temporarily store uploaded images.
- [ ] **OCR Processing:**
  - [ ] Implement a service to call the Azure Computer Vision API stub.
  - [ ] Process the OCR results and handle any errors.
- [ ] **LLM Integration:**
  - [ ] Implement a service that takes OCR text and calls the OpenAI API (or similar LLM) to extract product details.
  - [ ] Process JSON output to retrieve nutritional info, shelf life, food category, unit, and quantity.
  - [ ] Detect low-confidence outputs and trigger a fallback for manual input.
- [ ] **Cleanup:**
  - [ ] Ensure temporary images are deleted after processing.

## Phase 3: Frontend Components Development
- [ ] **Expiration Calendar UI:**
  - [ ] Create a Blazor component displaying a calendar view for the current week.
  - [ ] List expiring food items in descending order alongside dates.
  - [ ] Grey out past days.
  - [ ] Implement bulk edit functionality to mark items as "used".
- [ ] **Shopping History Page:**
  - [ ] Develop a Blazor page that displays a list of past purchases from Cosmos DB.
  - [ ] Group purchases by user and include details (name, expiration date, nutritional info, used status).
  - [ ] Add export functionality for shopping history (support JSON and CSV formats).

## Phase 4: OAuth Authentication Integration
- [ ] **Frontend Integration:**
  - [ ] Integrate OAuth providers (e.g., Microsoft, Google) into the Blazor project.
- [ ] **Backend Security:**
  - [ ] Secure backend API endpoints to restrict access to authenticated users.
  - [ ] Ensure user-specific data isolation for shopping history and receipt processing.

## Phase 5: Logging, Error Handling, and Testing
- [ ] **Logging:**
  - [ ] Integrate Azure Application Insights into both the backend and Blazor frontend.
- [ ] **Error Handling:**
  - [ ] Implement robust error handling for OCR, LLM calls, Blob Storage, and Cosmos DB interactions.
  - [ ] Develop user-friendly error messages and fallback mechanisms.
- [ ] **Testing:**
  - [ ] Write unit tests for:
    - [ ] LLM output validation.
    - [ ] OCR text extraction accuracy.
    - [ ] Database operations.
  - [ ] Create integration tests for:
    - [ ] End-to-end receipt processing workflow.
    - [ ] Authentication flow.
    - [ ] Bulk editing in the calendar view.

## Phase 6: Final Integration and Deployment
- [ ] **Module Integration:**
  - [ ] Wire the receipt upload, processing pipeline, expiration calendar, and shopping history together.
  - [ ] Verify that processed data correctly updates the UI.
  - [ ] Ensure secure authentication and logging are active across all modules.
- [ ] **End-to-End Testing:**
  - [ ] Conduct full end-to-end tests to validate the complete workflow.
  - [ ] Refine components based on test feedback.
- [ ] **Documentation and Deployment:**
  - [ ] Document project setup, testing instructions, and deployment steps.
  - [ ] Prepare the project for deployment to the chosen Azure resources.

