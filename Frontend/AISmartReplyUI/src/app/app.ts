import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SmartReplyService } from './smart-reply.service';
import { Component, ChangeDetectorRef} from '@angular/core';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet,FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  customerMessage: string = '';
  selectedTone: string = 'Professional';
  selectedReplyLength: string = 'Short';
  generatedReply: string = '';
  isGenerating: boolean = false;
  copyMessage: string = '';
  errorMessage: string = '';
  successMessage: string = '';

  constructor(private smartReplyService: SmartReplyService,
  private cdr: ChangeDetectorRef) {    
  }

  generateReply(): void {
    if (this.isGenerating) {
  return;
}
    this.successMessage = '';
    if(this.generatedReply!=''){
      this.generatedReply='';
      this.errorMessage = '';
    }
    if (!this.customerMessage.trim()) {
      alert('Please enter a customer message.');
      return;
      }
      else if (this.customerMessage.trim().length < 3) {
        alert('Please enter at least 3 characters.');
        return;
      }

    //console.log('Generate button clicked');
    if(this.generatedReply==''){
    this.isGenerating = true;
    this.copyMessage = '';
    this.cdr.detectChanges();
    }

  const request = {
    customerMessage: this.customerMessage,
    tone: this.selectedTone,
    replyLength: this.selectedReplyLength
  };

  console.log('Request:', request);

  this.smartReplyService.generateReply(request).subscribe({
    next: (response) => {
      console.log('API Response:', response);
      if (
  typeof response?.reply === 'string' &&
  response.reply.trim().length > 0
) {
  this.generatedReply = response.reply;
  this.successMessage = 'Reply generated successfully!';
} else {
  this.generatedReply = '';
  this.errorMessage = 'Invalid response received from server. Please try again.';
}
      this.isGenerating = false;
      this.cdr.detectChanges();
    },
    error: (error) => {
      if (error.status === 504 || error.status === 408)  {
  this.errorMessage = 'The request timed out. Please try again.';
}
  else if (error.status === 429) {
    this.errorMessage = 'You have reached the request limit. Please wait a little and try again..';
  } else if (error.status === 0) {
    this.errorMessage = 'Unable to connect to the server. Please try again later.';
  } 
  else if (error.status === 400 && error.error?.message) {
  this.errorMessage = error.error.message;
  }
  else {
    this.errorMessage = 'Something went wrong. Please try again.';
  }

  this.isGenerating = false;
  this.cdr.detectChanges();
}
  });

  // const request = {
  //   customerMessage: this.customerMessage,
  //   tone: this.selectedTone,
  //   replyLength: this.selectedReplyLength
  // };

  // this.smartReplyService.generateReply(request).subscribe({
  //   next: (response) => {
  //     this.generatedReply = response.reply;
  //   },
  //   error: (error) => {
  //     console.error('API Error:', error);
  //   }
  // });
}
copyReply(): void {
  navigator.clipboard.writeText(this.generatedReply)
    .then(() => {
      this.copyMessage = 'Copied successfully!';
      this.cdr.detectChanges();
    })
    .catch(() => {
      this.copyMessage = 'Unable to copy. Please try again.';
      this.cdr.detectChanges();
    });
}

clearForm(): void {
  this.customerMessage = '';
  this.selectedTone = 'Professional';
  this.selectedReplyLength = 'Short';
  this.generatedReply = '';
  this.isGenerating = false;
  this.copyMessage='';
  this.errorMessage='';
  this.successMessage='';
}
}
