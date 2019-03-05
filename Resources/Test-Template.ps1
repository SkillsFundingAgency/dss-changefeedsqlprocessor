# common variables
$ResourceGroupName = "dfc-test-template-rg"
$TemplateFile = "Resources\template.json"

Describe "ChangeFeedSqlProcessor ARM Template Tests" -Tag "Acceptance" {
  
  Context "When ARM Template deployed with all parameters" {
    $TemplateParameters = @{
        changeFeedProcessorPrefix = "dss-foo-chgfeed"
        serviceBusConnectionString = "Endpoint=sb://dss-foo-shared-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=thisisnotasecret="
        sharedAppServicePlanName = "dss-foo-shared-asp"
        sharedAppServicePlanResourceGroup = "dss-foo-shared-rg"
    }
    $TestTemplateParams = @{
      ResourceGroupName       = $ResourceGroupName
      TemplateFile            = $TemplateFile
      TemplateParameterObject = $TemplateParameters
    }

    $output = Test-AzureRmResourceGroupDeployment @TestTemplateParams
  
    It "Should be deployed successfully" {
      $output | Should -Be $null
    }

  }
}