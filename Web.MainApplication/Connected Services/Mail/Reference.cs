﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Web.MainApplication.Mail {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="StreamAttachment", Namespace="http://schemas.datacontract.org/2004/07/WCFSoapIndosuryaMailSender")]
    [System.SerializableAttribute()]
    public partial class StreamAttachment : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FileNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.IO.MemoryStream StreamField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FileName {
            get {
                return this.FileNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FileNameField, value) != true)) {
                    this.FileNameField = value;
                    this.RaisePropertyChanged("FileName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.IO.MemoryStream Stream {
            get {
                return this.StreamField;
            }
            set {
                if ((object.ReferenceEquals(this.StreamField, value) != true)) {
                    this.StreamField = value;
                    this.RaisePropertyChanged("Stream");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="Mail.IService1")]
    public interface IService1 {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/SendMail", ReplyAction="http://tempuri.org/IService1/SendMailResponse")]
        Web.MainApplication.Mail.SendMailResponse SendMail(Web.MainApplication.Mail.SendMailRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IService1/SendMail", ReplyAction="http://tempuri.org/IService1/SendMailResponse")]
        System.Threading.Tasks.Task<Web.MainApplication.Mail.SendMailResponse> SendMailAsync(Web.MainApplication.Mail.SendMailRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="SendMail", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class SendMailRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string[] to;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string subject;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=2)]
        public string body;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=3)]
        public string[] cc;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=4)]
        public string[] bcc;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=5)]
        public bool isIsBodyHtml;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=6)]
        public Web.MainApplication.Mail.StreamAttachment[] streamAttachment;
        
        public SendMailRequest() {
        }
        
        public SendMailRequest(string[] to, string subject, string body, string[] cc, string[] bcc, bool isIsBodyHtml, Web.MainApplication.Mail.StreamAttachment[] streamAttachment) {
            this.to = to;
            this.subject = subject;
            this.body = body;
            this.cc = cc;
            this.bcc = bcc;
            this.isIsBodyHtml = isIsBodyHtml;
            this.streamAttachment = streamAttachment;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="SendMailResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class SendMailResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public bool SendMailResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string errorMessage;
        
        public SendMailResponse() {
        }
        
        public SendMailResponse(bool SendMailResult, string errorMessage) {
            this.SendMailResult = SendMailResult;
            this.errorMessage = errorMessage;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IService1Channel : Web.MainApplication.Mail.IService1, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class Service1Client : System.ServiceModel.ClientBase<Web.MainApplication.Mail.IService1>, Web.MainApplication.Mail.IService1 {
        
        public Service1Client() {
        }
        
        public Service1Client(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public Service1Client(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public Service1Client(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Web.MainApplication.Mail.SendMailResponse Web.MainApplication.Mail.IService1.SendMail(Web.MainApplication.Mail.SendMailRequest request) {
            return base.Channel.SendMail(request);
        }
        
        public bool SendMail(string[] to, string subject, string body, string[] cc, string[] bcc, bool isIsBodyHtml, Web.MainApplication.Mail.StreamAttachment[] streamAttachment, out string errorMessage) {
            Web.MainApplication.Mail.SendMailRequest inValue = new Web.MainApplication.Mail.SendMailRequest();
            inValue.to = to;
            inValue.subject = subject;
            inValue.body = body;
            inValue.cc = cc;
            inValue.bcc = bcc;
            inValue.isIsBodyHtml = isIsBodyHtml;
            inValue.streamAttachment = streamAttachment;
            Web.MainApplication.Mail.SendMailResponse retVal = ((Web.MainApplication.Mail.IService1)(this)).SendMail(inValue);
            errorMessage = retVal.errorMessage;
            return retVal.SendMailResult;
        }
        
        public System.Threading.Tasks.Task<Web.MainApplication.Mail.SendMailResponse> SendMailAsync(Web.MainApplication.Mail.SendMailRequest request) {
            return base.Channel.SendMailAsync(request);
        }
    }
}
