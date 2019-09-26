import {Injectable} from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

// stops you from navigating away when you have made changes
@Injectable()
export class PreventUnsavedChanges implements CanDeactivate<MemberEditComponent> {
    canDeactivate(component: MemberEditComponent) {
      if (component.editForm) {
          return confirm('Are you sure you want to continue? Any unsaved changes will be lost');
      }
      return true;
    }

}
