import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Task, TaskPriority } from '../../../core/models/task.model';
import { TaskService } from '../../../core/services/task.service';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent implements OnInit {
  @Input() task: Task | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  form: FormGroup;
  isSaving = false;
  errorMessage: string | null = null;
  priorities: TaskPriority[] = ['Low', 'Medium', 'High'];

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      priority: ['Medium', Validators.required],
      dueDate: ['']
    });
  }

  ngOnInit(): void {
    if (this.task) {
      this.form.patchValue({
        title: this.task.title,
        description: this.task.description ?? '',
        priority: this.task.priority,
        dueDate: this.task.dueDate ?? ''
      });
    }
  }

  get isEditMode(): boolean {
    return !!this.task;
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = null;

    const value = this.form.getRawValue();
    const payload = {
      title: value.title,
      description: value.description || undefined,
      priority: value.priority,
      dueDate: value.dueDate || undefined
    };

    const onSuccess = () => {
      this.isSaving = false;
      this.saved.emit();
    };

    const onError = () => {
      this.isSaving = false;
      this.errorMessage = 'Failed to save task. Please check your input.';
    };

    if (this.isEditMode) {
      this.taskService.update(this.task!.id, payload).subscribe({
        next: onSuccess,
        error: onError
      });
    } else {
      this.taskService.create(payload).subscribe({
        next: onSuccess,
        error: onError
      });
    }
  }

  onCancel(): void {
    this.cancelled.emit();
  }
}